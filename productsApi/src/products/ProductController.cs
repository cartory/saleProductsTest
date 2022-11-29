using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using src.utils;

namespace productsApi.src.products
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        public readonly IProductService productService;
        int internalServerErrorCode = Convert.ToInt32(HttpStatusCode.InternalServerError);

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet]
        public ActionResult<Response<IEnumerable<Product>>> GetProducts()
        {
            var res = new Response<IEnumerable<Product>>();

            try
            {
                res.data = productService.FindAll();

                return Ok(res);
            }
            catch (Exception e)
            {
                res.errorMessage = e.Message;
                return StatusCode(internalServerErrorCode, res);
            }
        }

        [HttpGet("{productId}")]
        public ActionResult<Response<Product>> GetProduct([FromRoute]string productId) 
        {
            var res = new Response<Product>();

            try
            {
                Product product = productService.FindById(productId);

                if (product == null)
                {
                    return NotFound(res);
                }

                res.data = product;
                return Ok(res);
            }
            catch (Exception e)
            {
                res.errorMessage = e.Message;
                return StatusCode(internalServerErrorCode, res);
            }
        }

        [HttpPost]
        public ActionResult<Response<bool>> CreateProduct([FromBody] Product product)
        {
            var res = new Response<bool>();

            try
            {
                bool validProduct = Validator.ValidateAtributes(product, out string errorMessage);

                if (!validProduct)
                {
                    res.errorMessage = errorMessage;
                    return BadRequest(res);
                }

                validProduct = product.ValidateImages(out errorMessage);
                if (!validProduct)
                {
                    res.errorMessage = errorMessage;
                    return BadRequest(res);
                }

                var wasInserted = this.productService.Create(product);
                res.data = wasInserted;

                return Ok(res);
            }
            catch (Exception e)
            {
                res.data = false;
                res.errorMessage = e.Message;

                return StatusCode(internalServerErrorCode, res);
            }
        }

        [HttpPut("{productId}")]
        public ActionResult<Response<bool>> UpdateProduct(
            [FromBody] Product product,
            [FromRoute] string productId
        ) 
        {
            var res = new Response<bool>();

            try
            {
                bool validProduct = Validator.ValidateAtributes(product, out string errorMessage);

                if (!validProduct)
                {
                    res.errorMessage = errorMessage;
                    return BadRequest(res);
                }

                validProduct = product.ValidateImages(out errorMessage);
                if (!validProduct) 
                {
                    res.errorMessage = errorMessage;
                    return BadRequest(res);
                }

                var wasInserted = this.productService.Update(productId, product);
                res.data = wasInserted;

                return Ok(res);
            }
            catch (Exception e)
            {
                res.data = false;
                res.errorMessage = e.Message;

                return StatusCode(internalServerErrorCode, res);
            }
        }

        [HttpDelete("{productId}")]
        public ActionResult<Response<bool>> UpdateProduct([FromRoute] string productId)
        {
            var res = new Response<bool>();

            try
            {
                var wasInserted = productService.Delete(productId);
                res.data = wasInserted;

                return Ok(res);
            }
            catch (Exception e)
            {
                res.data = false;
                res.errorMessage = e.Message;

                return StatusCode(internalServerErrorCode, res);
            }
        }
    }
}
