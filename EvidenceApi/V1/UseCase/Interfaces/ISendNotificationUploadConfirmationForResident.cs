using System;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ISendNotificationUploadConfirmationForResident
    {
        public void Execute(Guid evidenceRequestId);
    }
}
