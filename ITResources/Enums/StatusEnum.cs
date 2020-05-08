namespace InstaTransfer.ITResources.Enums
{
    public enum UmbrellaUserStatus
    {
        Active = 1,
        Inactive = 2,
        ChangePassword = 3,
        Blocked = 4,
        InUse = 5,
        PasswordExpired = 6,
        DecryptError = 7,
        InvalidBankAccount = 8,
        Timeout = 9,
        InvalidUsernamePassword = 10,
        PlatformError = 11
    }

    public enum CommerceUserStatus
    {
        Active = 1,
        Inactive = 2
    }

    public enum CommerceStatus
    {
        Active = 1,
        Inactive = 2
    }

    public enum DeclarationStatus
    {
        ReconciliationPending = 1,
        Reconciled = 2,
        Annulled = 3
    }

    public enum PurchaseOrderStatus
    {
        DeclarationPending = 1,
        Declared = 2,
        Annulled = 3,
        DeclaredReconciled = 4
    }

    public enum CashOutStatus
    {
        Pending = 1,
        Annulled = 2,
        Rejected = 3,
        Approved = 4,
        Completed = 5
    }
    public enum PaymentRequestStatus
    {
        Pending = 1,
        Annulled = 2,
        Declared = 3,
        DeclaredReconciled = 4
    }

    public enum ChargeAccountStatus
    {
        Pending = 1,   
        Accepted = 2,
        Rejected = 3,
        Registered = 4,
        Uploaded = 5
    }
}
