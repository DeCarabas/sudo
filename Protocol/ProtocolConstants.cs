namespace sudo.Protocol
{
    using System.Security.Principal;

    static class ProtocolConstants
    {
        public static string PipeName
        {
            get { return "Sudo-Commands-For-" + WindowsIdentity.GetCurrent().User.ToString(); }
        }
    }
}
