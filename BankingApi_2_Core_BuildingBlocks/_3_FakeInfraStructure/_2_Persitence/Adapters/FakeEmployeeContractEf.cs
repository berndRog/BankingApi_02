using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_FakeInfraStructure._2_Persitence.Adapters;

internal class FakeEmployeeContractEf(
): IEmployeeContract {

   public async Task<Result<EmployeeContractDto>> GetEmployeeBySubjectAsync(
      string subject,
      CancellationToken ct = default
   ) {
      // ALWAYS WALTER WAGNER! 
      var employeeContractDto =  new EmployeeContractDto(
         Id: Guid.Parse("00000000-0002-0000-0000-000000000000"),
         AdminRightsInt: 511 // all AdminRights
      );
      
      return Result<EmployeeContractDto>.Success(employeeContractDto);
      
   }
   
   public async Task<Result<EmployeeContractDto>> GetAuthorizedEmployeeAsync(
      AdminRights requiredRights,
      CancellationToken ct = default
   ) {
      // ALWAYS WALTER WAGNER! (for testing purposes, we don't have a real identity gateway
      var employeeContractDto = new EmployeeContractDto(
         Id: Guid.Parse("00000000-0002-0000-0000-000000000000"),
         AdminRightsInt: 511 // all AdminRights)
      );
      return Result<EmployeeContractDto>.Success(employeeContractDto);
   }

}