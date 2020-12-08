using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class CreateEvidenceRequestUseCase : ICreateEvidenceRequestUseCase
    {
        private readonly IEvidenceRequestValidator _validator;
        public CreateEvidenceRequestUseCase(IEvidenceRequestValidator validator)
        {
            _validator = validator;
        }

        public async Task<EvidenceRequestResponse> ExecuteAsync(EvidenceRequestRequest request)
        {
            var validation = _validator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }

            // TODO: Remove this when we have asynchronous database tasks to perform
            await Task.Delay(100).ConfigureAwait(true);

            return new EvidenceRequestResponse();
        }
    }
}
