using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
namespace BankingApi._2_Core.BuildingBlocks._4_BcContracts._1_Ports;

public interface IEmployeeContract {

   // Returns the currently authenticated employee
   // and verifies that the employee has the required administrative rights
   Task<Result<EmployeeContractDto>> GetEmployeeBySubjectAsync(
      string subject,
      CancellationToken ct = default
   );
   
   // Returns the currently authenticated employee
   // and verifies that the employee has the required administrative rights
   Task<Result<EmployeeContractDto>> GetAuthorizedEmployeeAsync(
      AdminRights adminRights,
      CancellationToken ct = default
   );

}
