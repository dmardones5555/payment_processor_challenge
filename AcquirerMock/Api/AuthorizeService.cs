using Api.Contracts;

namespace Api
{
    public class AuthorizeService
    {
        public static async Task<IResult> Handle(AuthorizeRequest request)
        {
            if (request.CardLast4 == "1111")
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

            if (request.CardLast4 == "2222")
            {
                return Results.Ok(new AuthorizeResponse
                {
                    Status = "DECLINED",
                    ResponseCode = "05",
                    ResponseMessage = "Do not honor"
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
