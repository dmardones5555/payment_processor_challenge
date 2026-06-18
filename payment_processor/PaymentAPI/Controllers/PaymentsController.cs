using Application.Payments.Commands.CreatePayment;
using Application.Payments.Queries.GetPayment;
using Application.Payments.Queries.SearchPayments;
using Microsoft.AspNetCore.Mvc;
using PaymentAPI.Contracts.Requests;

namespace PaymentAPI.Controllers
{
    public class PaymentsController : ControllerBase
    {
        private CreatePaymentHandler _handler;
        private GetPaymentHandler _getPaymentHandler;
        private SearchPaymentHandler _searchPaymentHandler;
        public PaymentsController(CreatePaymentHandler handler, GetPaymentHandler getPaymentHandler, SearchPaymentHandler searchPaymentHandler) 
        {
            _handler = handler;
            _getPaymentHandler = getPaymentHandler;
            _searchPaymentHandler = searchPaymentHandler;
        }

        [HttpPost]
        [Route("payments")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest paymentRequest, CancellationToken cancellationToken)
        {
            var command = new CreatePaymentCommand()
            {
                MerchantId = paymentRequest.MerchantId,
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency,
                Card = new CardCommand
                {
                    Number = paymentRequest.Card.Number,
                    Expiry = paymentRequest.Card.Expiry,
                    Cvv = paymentRequest.Card.Cvv
                },
                IdempotencyKey = paymentRequest.IdempotencyKey
            };

            var result = await _handler.Handle(command, cancellationToken);            
            return Ok(result);
        }

        [HttpGet]
        [Route("payments/{id}")]
        public async Task<IActionResult> GetPayment(Guid id, CancellationToken cancellationToken)
        {           
            var result = await _getPaymentHandler.Handle(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("payments")]
        public async Task<IActionResult> GetPayments([FromQuery] string? merchant_id, [FromQuery] string? status, CancellationToken cancellationToken)
        {
            var query = new SearchPaymentQuery
            {
                MerchantId = merchant_id,
                Status = status
            };

            var result = await _searchPaymentHandler.Handle(query, cancellationToken);
            return Ok(result);
        }
    }
}
