namespace WelpenWache.Core;

public class Policies {
    public class Admin {
        public const string CanManageUsers = "CanManageUsers";
    }
    
    public class Intern {
        public const string CanCreate = "CanCreateIntern";
        public const string CanDelete = "CanDeleteIntern";
        public const string CanUpdate = "CanUpdateIntern";
        public const string CanRead = "CanReadIntern";
    }
    
}