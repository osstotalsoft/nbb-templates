using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Application
{
    public class HelloWorld
    {
        public record Command :  IRequest;

        public class Handler : IRequestHandler<Command>
        {
            public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
