﻿using PactNet.Models.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PactNet.Mocks
{
    public interface IMockMessager
    {
        void ExceptsToRecieve(string messageTopic);

        void AddMessage(Message message);

        /// <summary>
        /// Gets the first message from queue.
        /// </summary>
        /// <returns></returns>
        Message GetMessage();
    }
}