namespace InstaTransfer.ITResources.Enums
{
    public enum SocialReasonType
    {
        PersonaNatural = 1,
        PersonaJuridica = 2
    }

    public enum BankStatementEntryType
    {
        Credit = 1,
        Debit = 2
    }

    public enum CommerceBalanceType
    {
        Declaration = 1,
        CashOut = 2
    }
    public enum OperationType
    {
        Transfer = 1,
        Deposit = 2
    }

    public enum EmailType
    {
        NewPaymentRequest = 1,
        DeclarationSuccess = 2,
        ReconciliationSuccess = 3,
        RecoverCommerceUserPW = 4,
        RecoverEndUserPW = 5,
        SmsFailure = 6
    }

    public enum ReconciliationType
    {
        Automatic = 1,
        Manual = 2,
        RealTime = 3,
        Api = 4,
    }

    public enum UserType
    {
        OnlineBanking = 1,
        MassiveBankingUploads = 2,
        MassivaBankingConsults = 3
    }
}
