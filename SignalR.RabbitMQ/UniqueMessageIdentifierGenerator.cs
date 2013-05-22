using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Tracing;

namespace SignalR.RabbitMQ
{
    /*
     We need to generate a globally unique identifier for our messages.
     
     Instead of using a global value we instead use the side effect of the fact that a 
     scale out message bus sees every value in the system.
     
     We seed the initital value from the current tick value.
     
     If we see a number higher than this we increment the value to the last value we have seen.
     If we see the same value we increment by 1 the value we have.
     
     Eventually we will see a number higher than the one we have been generating so we will use that.
     
     The effect of this kungfu will be that every server will keep the messages in the correct order for the same hub/user combination.
     The fact server times are never synced will also be mitigated?
     
     Caveat
     -----
     If you are generating more than a 10,000 messages every millisecond you are in trouble.
     
     */
    internal class UniqueMessageIdentifierGenerator
    {
        private static ulong _lastSeenMessageIdentifier;
        private readonly TraceSource _trace;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public UniqueMessageIdentifierGenerator(IDependencyResolver resolver)
        {
            var traceManager = resolver.Resolve<ITraceManager>();
            _trace = traceManager["SignalR.RabbitMQ." + typeof(UniqueMessageIdentifierGenerator).Name];

            _trace.TraceEvent(TraceEventType.Information, 0, "UniqueMessageIdentifierGenerator instance created.");
            _lastSeenMessageIdentifier = GenerateValue();
        }

        public void LastSeenMessageIdentifier(ulong identifier)
        {
            try
            {
                _lock.EnterWriteLock();
                if (identifier > _lastSeenMessageIdentifier)
                {
                    _trace.TraceEvent(TraceEventType.Information, 0, string.Format("Updating _lastSeenMessageIdentifier {0}.", identifier));
                    _lastSeenMessageIdentifier = identifier;
                }
                else if (identifier == _lastSeenMessageIdentifier)
                { 
                    _lastSeenMessageIdentifier++;
                    _trace.TraceEvent(TraceEventType.Information, 0, string.Format("Incremented _lastSeenMessageIdentifier {0}.", _lastSeenMessageIdentifier));
                }
                //loose the value
            }finally
            {
                _lock.ExitWriteLock();   
            }
        }

        public ulong GetNextMessageIdentifier()
        {
            try
            {
                _lock.EnterWriteLock();

                var toReturn = GenerateValue();
                if (toReturn <= _lastSeenMessageIdentifier)
                {
                    var latest = _lastSeenMessageIdentifier++;
                    _lastSeenMessageIdentifier = latest;
                    _trace.TraceEvent(TraceEventType.Information, 0, string.Format("Returning incremented {0}.", latest));
                    return latest;
                }
                _trace.TraceEvent(TraceEventType.Information, 0, string.Format("Returning generated value {0}.", toReturn));
                _lastSeenMessageIdentifier = toReturn;
                return toReturn;
            }finally
            {
                _lock.ExitWriteLock();
            }
        }

        private static ulong GenerateValue()
        {
            return (ulong)DateTime.UtcNow.Ticks;
        }
    }
}