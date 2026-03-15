// <copyright file="GrpcPatientService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using IPatientBusinessService = Aloe.Apps.Medock.MdPatientLib.Services.IPatientService;

namespace Aloe.Apps.Medock.MdPatientApi.Grpc;

/// <summary>
/// 患者サービスの MagicOnion gRPC 実装。
/// </summary>
public class GrpcPatientService : ServiceBase<IPatientService>, IPatientService
{
    private readonly IPatientBusinessService _patientService;

    /// <summary>
    /// <see cref="GrpcPatientService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcPatientService(IPatientBusinessService patientService)
    {
        _patientService = patientService;
    }

    /// <inheritdoc/>
    public async UnaryResult<SearchPatientsResponse> SearchAsync(SearchPatientsRequest request)
    {
        var result = await _patientService.SearchAsync(request);
        return result;
    }

    /// <inheritdoc/>
    public async UnaryResult<PatientDetailResponse> GetByIdAsync(GetPatientRequest request)
    {
        var result = await _patientService.GetByIdAsync(request.PtId);
        if (result == null)
        {
            throw new ReturnStatusException(StatusCode.NotFound, $"患者 '{request.PtId}' が見つかりません。");
        }

        return result;
    }

    /// <inheritdoc/>
    public async UnaryResult<PatientDetailResponse> CreateAsync(CreatePatientRequest request)
    {
        var result = await _patientService.CreateAsync(request);
        return result;
    }

    /// <inheritdoc/>
    public async UnaryResult<PatientDetailResponse> UpdateAsync(UpdatePatientRequest request)
    {
        var result = await _patientService.UpdateAsync(request);
        if (result == null)
        {
            throw new ReturnStatusException(StatusCode.NotFound, $"患者 '{request.PtId}' が見つかりません。");
        }

        return result;
    }

    /// <inheritdoc/>
    public async UnaryResult<DeletePatientResponse> DeleteAsync(DeletePatientRequest request)
    {
        var success = await _patientService.DeleteAsync(request.PtId);
        return new DeletePatientResponse { Success = success };
    }
}
