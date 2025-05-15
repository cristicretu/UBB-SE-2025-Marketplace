using System;

namespace SharedClassLibrary.Service
{
    public interface IAuthorizationService
    {
        string GenerateJwtToken();
    }
}
