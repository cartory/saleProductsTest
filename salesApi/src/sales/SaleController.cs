using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using src.utils;
using RabbitMQ.Client;

namespace salesApi.src.sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : ControllerBase
    {
        public readonly ISaleService saleService;
        public int internalServerErrorCode = Convert.ToInt32(HttpStatusCode.InternalServerError);

        public SaleController(ISaleService saleService) 
        { 
            this.saleService = saleService;
        }

        [HttpGet]
        public ActionResult<Response<IEnumerable<SaleRes>>> GetSales() 
        {
            var res = new Response<IEnumerable<SaleRes>>();
            
            try
            {
                IEnumerable<SaleRes> sales = saleService.FindAll();
                res.data = sales;

                return Ok(res);
            }
            catch (Exception e)
            {
                res.errorMessage = e.Message;
                return StatusCode(internalServerErrorCode, res);
            }
        }
        
        [HttpGet("{saleId}")]
        public async Task<ActionResult<Response<SaleRes>>> GetAndForceUpdateSaleStateAsync([FromRoute] string saleId) 
        {
            var res = new Response<SaleRes>();

            try
            {
                res.data = await saleService.FindAndForceUpdateSaleState(saleId);
                return Ok(res);
            }
            catch (Exception e)
            {
                res.errorMessage = e.Message;
                return StatusCode(internalServerErrorCode, res);
            }
        }

        [HttpPost]
        public ActionResult<Response<string>> PostNewSalePendingState
        (
            [FromBody] SaleReq sale        
        ) 
        {
            var res = new Response<string>();

            lock (sale)
            {
                try
                {
                    string insertedId = saleService.AddNewSalePending(sale);
                    res.data = insertedId;

                    return Ok(res);
                }
                catch (Exception e)
                {
                    res.data = "";
                    res.errorMessage = e.Message;
                    return StatusCode(internalServerErrorCode, res);
                }
            }
        }
    }
}
