using System;
using System.Net.Http;

namespace Philosopher.Multiplat.Models
{
    public struct ResultOrErrorResponse<T>
    {
        private readonly T _result;
        public T Result
        {
            get
            {
                if (IsResult)
                {
                    return _result;
                }
                else
                {
                    throw new Exception("Attempted to get empty result.");
                }
            }
        }

        private readonly GenericHttpResponse _errorResponse;
        public GenericHttpResponse ErrorResponse
        {
            get
            {
                if (IsError)
                {
                    return _errorResponse;
                }
                else
                {
                    throw new Exception("Attempted to get empty error.");
                }
            }
        }

        public bool IsError => _errorResponse != null;
        public bool IsResult => _errorResponse == null;     

        public ResultOrErrorResponse(T result)
        {
            _result = result;
            _errorResponse = null;
        }

        public ResultOrErrorResponse(GenericHttpResponse error)
        {
            _errorResponse = error;
            _result = default(T);
        }        
    }
}