using System;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface ISendNotificationUploadConfirmationToResidentAndStaff
    {
        public void Execute(Guid evidenceRequestId);
    }
}
