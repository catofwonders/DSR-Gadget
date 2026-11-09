using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSR_Gadget
{
    /// <summary>
    /// A switchable change to the game's code, found by an AOB scan.
    /// Either overwrites the original instructions in place, or jumps to a small block of new code
    /// (a "code cave") that runs extra instructions, then the original ones, then jumps back.
    /// Ported from the Cheat Engine scripts in Phokz's DSR table.
    /// </summary>
    internal class CodePatch
    {
        private readonly PHook hook;
        private readonly PHPointer location;
        private readonly byte[] originalBytes;
        private readonly byte[] inPlaceBytes;
        private readonly byte[] cavePrefix;
        private readonly bool runOriginal;
        private readonly object sync = new object();

        private IntPtr patchedAddress = IntPtr.Zero;
        private IntPtr cave = IntPtr.Zero;

        public bool Applied => patchedAddress != IntPtr.Zero;

        /// <summary>
        /// Overwrites the original bytes with replacement bytes of the same length.
        /// </summary>
        public static CodePatch InPlace(PHook hook, PHPointer location, byte[] originalBytes, byte[] replacementBytes)
        {
            if (originalBytes.Length != replacementBytes.Length)
                throw new ArgumentException("In-place patches must keep the same length");
            return new CodePatch(hook, location, originalBytes, replacementBytes, null);
        }

        /// <summary>
        /// Runs prefix bytes before the original instructions (or instead of them, if runOriginal is false).
        /// The original instructions must be position-independent (no RIP-relative addressing or
        /// relative jumps) and at least 5 bytes long.
        /// </summary>
        public static CodePatch Cave(PHook hook, PHPointer location, byte[] originalBytes, byte[] prefixBytes, bool runOriginal = true)
        {
            if (originalBytes.Length < 5)
                throw new ArgumentException("Need at least 5 bytes of original code to place a jump");
            return new CodePatch(hook, location, originalBytes, null, prefixBytes, runOriginal);
        }

        private CodePatch(PHook hook, PHPointer location, byte[] originalBytes, byte[] inPlaceBytes, byte[] cavePrefix, bool runOriginal = true)
        {
            this.runOriginal = runOriginal;
            this.hook = hook;
            this.location = location;
            this.originalBytes = originalBytes;
            this.inPlaceBytes = inPlaceBytes;
            this.cavePrefix = cavePrefix;
        }

        /// <summary>
        /// Offset in the cave where the 16 spare data bytes start.
        /// </summary>
        public int CaveDataOffset => (cavePrefix?.Length ?? 0) + (runOriginal ? originalBytes.Length : 0) + 5;

        /// <summary>
        /// Turns the patch on. Returns false if the game's code isn't what we expect
        /// (different game version, or another tool already changed it).
        /// </summary>
        public bool Apply()
        {
            lock (sync)
            {
                if (Applied)
                    return true;
                if (!hook.Hooked)
                    return false;

                IntPtr address = location.Resolve();
                if (address == IntPtr.Zero)
                    return false;

                byte[] current = Kernel32.ReadBytes(hook.Handle, address, (uint)originalBytes.Length);
                if (current == null || !current.SequenceEqual(originalBytes))
                    return false;

                if (inPlaceBytes != null)
                {
                    if (!Kernel32.WriteBytes(hook.Handle, address, inPlaceBytes))
                        return false;
                    patchedAddress = address;
                    return true;
                }

                // Cave layout: prefix, original instructions (optional), jmp back, then 16 spare bytes for data
                int jmpBackAt = cavePrefix.Length + (runOriginal ? originalBytes.Length : 0);
                int caveSize = jmpBackAt + 5 + 16;
                IntPtr newMem = hook.AllocateNearbyMemory(address, (IntPtr)caveSize, Kernel32.PAGE_EXECUTE_READWRITE);
                if (newMem == IntPtr.Zero)
                    return false;

                List<byte> caveBytes = new List<byte>(caveSize);
                caveBytes.AddRange(cavePrefix);
                if (runOriginal)
                    caveBytes.AddRange(originalBytes);
                caveBytes.Add(0xE9);
                caveBytes.AddRange(BitConverter.GetBytes(relativeJump(newMem + jmpBackAt, address + originalBytes.Length)));
                caveBytes.AddRange(new byte[16]);
                Kernel32.WriteBytes(hook.Handle, newMem, caveBytes.ToArray());

                byte[] jump = new byte[originalBytes.Length];
                jump[0] = 0xE9;
                Array.Copy(BitConverter.GetBytes(relativeJump(address, newMem)), 0, jump, 1, 4);
                for (int i = 5; i < jump.Length; i++)
                    jump[i] = 0x90;

                if (!Kernel32.WriteBytes(hook.Handle, address, jump))
                {
                    hook.Free(newMem);
                    return false;
                }

                cave = newMem;
                patchedAddress = address;
                return true;
            }
        }

        /// <summary>
        /// Turns the patch off and restores the game's original code.
        /// </summary>
        public void Remove()
        {
            lock (sync)
            {
                if (!Applied)
                    return;
                if (hook.Hooked)
                {
                    Kernel32.WriteBytes(hook.Handle, patchedAddress, originalBytes);
                    // The game may be running the cave code this very moment, so it's left allocated;
                    // it's a few bytes and goes away when the game closes.
                }
                patchedAddress = IntPtr.Zero;
                cave = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Forgets the patch without touching memory, for when the game has closed.
        /// </summary>
        public void Forget()
        {
            lock (sync)
            {
                patchedAddress = IntPtr.Zero;
                cave = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Writes bytes into the cave at the given offset (used for values the cave code reads).
        /// </summary>
        public void WriteCave(int offset, byte[] bytes)
        {
            lock (sync)
            {
                if (cave != IntPtr.Zero && hook.Hooked)
                    Kernel32.WriteBytes(hook.Handle, cave + offset, bytes);
            }
        }

        private static int relativeJump(IntPtr from, IntPtr to)
        {
            long distance = to.ToInt64() - (from.ToInt64() + 5);
            if (distance > int.MaxValue || distance < int.MinValue)
                throw new InvalidOperationException("Jump target is too far away");
            return (int)distance;
        }
    }
}
