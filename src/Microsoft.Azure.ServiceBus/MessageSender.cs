﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class MessageSender : ClientEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1126:PrefixCallsCorrectly",
            Justification = "This is not a method call, but a type.")]
        protected MessageSender(TimeSpan operationTimeout)
            : base(nameof(MessageSender) + StringUtility.GetRandomString())
        {
            this.OperationTimeout = operationTimeout;
        }

        internal TimeSpan OperationTimeout { get; }

        protected MessagingEntityType EntityType { get; set; }

        public Task SendAsync(BrokeredMessage brokeredMessage)
        {
            return this.SendAsync(new BrokeredMessage[] { brokeredMessage });
        }

        public async Task SendAsync(IEnumerable<BrokeredMessage> brokeredMessages)
        {
            int count = MessageSender.ValidateMessages(brokeredMessages);
            MessagingEventSource.Log.MessageSendStart(this.ClientId, count);

            try
            {
                await this.OnSendAsync(brokeredMessages).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                MessagingEventSource.Log.MessageSendException(this.ClientId, exception);
                throw;
            }

            MessagingEventSource.Log.MessageSendStop(this.ClientId);
        }

        protected abstract Task OnSendAsync(IEnumerable<BrokeredMessage> brokeredMessages);

        static int ValidateMessages(IEnumerable<BrokeredMessage> brokeredMessages)
        {
            int count = 0;

            if (brokeredMessages == null)
            {
                throw Fx.Exception.ArgumentNull(nameof(brokeredMessages));
            }
            foreach (var message in brokeredMessages)
            {
                count++;
                if (message.IsLockTokenSet)
                {
                    throw Fx.Exception.Argument(nameof(brokeredMessages), "Cannot Send ReceivedMessages");
                }
            }

            if (count == 0)
            {
                throw Fx.Exception.Argument(nameof(brokeredMessages), "BrokeredMessage List cannot be empty");
            }

            return count;
        }
    }
}