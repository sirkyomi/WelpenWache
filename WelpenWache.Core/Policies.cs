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

    public class Team {
        public const string CanCreate = "CanCreateTeam";
        public const string CanDelete = "CanDeleteTeam";
        public const string CanUpdate = "CanUpdateTeam";
        public const string CanRead = "CanReadTeam";
    }
    
}
