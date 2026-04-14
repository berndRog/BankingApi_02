namespace BankingApi._2_Core.Customers._3_Domain.Enum;

// Reasons why a customer application can be rejected.
// Represents domain-level rejection codes used during customer onboarding.
public enum RejectCode {
   None = 0,

   // Identity verification failed
   IdentityVerificationFailed = 1,

   // Customer already exists (duplicate email / identity)
   DuplicateCustomer = 2,

   // Blacklist or sanction list match
   BlacklistedPerson = 3,

   // Invalid or unverifiable address
   InvalidAddress = 4,

   // Failed compliance / KYC checks
   ComplianceCheckFailed = 5,

   // Customer did not complete activation process
   ActivationTimeout = 6,

   // Manual rejection by bank employee
   ManuallyRejected = 7
}