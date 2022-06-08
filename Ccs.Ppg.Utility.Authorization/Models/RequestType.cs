namespace  Ccs.Ppg.Utility.Authorization
{
    public enum RequestType
    {
        HavingOrgId, // Organisation, Sie, Org/Site contacts
        NotHavingOrgId, // User
        Other // Individual contacts. Not used in UI
    }
}