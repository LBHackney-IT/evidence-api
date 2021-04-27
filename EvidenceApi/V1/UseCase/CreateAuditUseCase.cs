using System;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class CreateAuditUseCase : ICreateAuditUseCase
    {
        public void Execute(string path, string hackneyToken)
        {
            Console.WriteLine("Path: {0}, HackneyToken: {1}", path, hackneyToken);
        }
    }
}
