using Api.Contracts;

namespace Api
{
    public class AuthorizeService
    {
        public static async Task<IResult> Handle(AuthorizeRequest request)
        {
            int[] probs = { 1, 1, 1, 1, 1, 2, 2, 3, 4, 5 };
            var random = new Random();
            var select = random.Next(0,11);
            int prob = probs[select];

            if (prob == 1)
            {
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "APPROVED",
                    AuthorizationCode = "AUTH123",
                    AcquirerReference = $"ACQ-{Guid.NewGuid():N}"[..12],
                    ResponseCode = "00",
                    ResponseMessage = "Approved"
                });
            }

            if (prob == 2)
            {
                Thread.Sleep(2000);
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "APPROVED",
                    AuthorizationCode = "AUTH123",
                    AcquirerReference = $"ACQ-{Guid.NewGuid():N}"[..12],
                    ResponseCode = "00",
                    ResponseMessage = "Approved"
                });
            }

            if (prob == 3)
            {
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "DECLINED",
                    ResponseCode = "51",
                    ResponseMessage = "Insufficient funds"
                });
            }

            if (prob == 4)
            {
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "DECLINED",
                    ResponseCode = "05",
                    ResponseMessage = "Transaction declined"
                });
            }

            if (prob == 5)
            {
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "DECLINED",
                    ResponseCode = "04",
                    ResponseMessage = "Retain card"
                });
            }

            return Results.Ok(new AuthorizeResponse
            {
                Status = "APPROVED",
                AuthorizationCode = "AUTH999",
                AcquirerReference = $"ACQ-{Guid.NewGuid():N}"[..12],
                ResponseCode = "00",
                ResponseMessage = "Approved"
            });
        }
    }
}
