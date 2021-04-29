using System;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class CreateAuditUseCase : ICreateAuditUseCase
    {
        public void Execute(string path, string method, string queryString, string hackneyToken)
        {
            Console.WriteLine("Path: {0}, Method: {1}, QueryString: {2}, HackneyToken: {3}", path, method, queryString, hackneyToken);
        }
    }
}
