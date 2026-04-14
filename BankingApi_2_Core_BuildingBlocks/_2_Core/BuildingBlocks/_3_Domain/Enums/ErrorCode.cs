namespace BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;

public enum ErrorCode: Int32 {
   Ok = 200,
   BadRequest = 400,
   Unauthorized = 401,
   Forbidden = 403,
   NotFound = 404,
   Conflict = 409,
   UnsupportedMediaType = 415,
   UnprocessableEntity = 422,
}