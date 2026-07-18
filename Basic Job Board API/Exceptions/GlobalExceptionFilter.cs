using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JobBoardAPI.Exceptions
{
	public class GlobalExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<GlobalExceptionFilter> _logger;
		public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			var statusCode = context.Exception switch
			{
				KeyNotFoundException => StatusCodes.Status404NotFound,
				UnauthorizedAccessException => StatusCodes.Status403Forbidden,
				InvalidOperationException => StatusCodes.Status400BadRequest,
				_ => StatusCodes.Status500InternalServerError
			};
			_logger.LogError(context.Exception, context.Exception.Message);

			context.Result = new ObjectResult(new
			{
				error = context.Exception.Message,
				statusCode
			})
			{
				StatusCode = statusCode
			};

			context.ExceptionHandled = true;
		}
	}
}
