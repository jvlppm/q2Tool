using System.Collections.Generic;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;
using System;
using Jv.Networking;

namespace q2Tool
{
    public class Package<CommandsType> : ICommand where CommandsType : ICommand
    {
        public Queue<CommandsType> Commands { get; private set; }
        public List<byte> RemainingData { get; set; }

        public Package()
        {
            Commands = new Queue<CommandsType>();
            RemainingData = new List<byte>();
        }

        public void Clear()
        {
            Commands.Clear();
            RemainingData.Clear();
        }

        #region ICommand
        public int Size()
        {
            int size = 0;

            foreach (ICommand cmd in Commands)
                size += cmd.Size();

            return size + RemainingData.Count;
        }

        public void WriteTo(RawData data)
        {
            foreach (ICommand cmd in Commands)
                cmd.WriteTo(data);

            if (RemainingData.Count != 0)
                data.WriteBytes(RemainingData.ToArray());
        }
        #endregion
    }
}