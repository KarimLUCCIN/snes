using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Memory
{
    /// <summary>
    /// Espace mémoire pouvant être addressé, exécuté, etc
    /// </summary>
    public class MemoryContainer
    {
        public byte[] Data;

        public int Length
        {
            get { return Data.Length; }
        }

        /// <summary>
        /// Crée un nouveau conteneur de mémoire à partir d'un contenu existant
        /// </summary>
        /// <param name="data"></param>
        public MemoryContainer(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.Data = data;
        }

        /// <summary>
        /// Crée un nouveau conteneur vide avec la taille spécifiée
        /// </summary>
        /// <param name="size"></param>
        public MemoryContainer(int size)
        {
            if (size <= 1024)
                throw new InvalidOperationException("Au moins 1024 bytes doivent être alloués");

            Data = new byte[size];
        }
    }
}
