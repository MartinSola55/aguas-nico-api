namespace AguasNico_Api.Models.Constants;

public class Messages
{
    public class Error
    {
        public static string Exception() => "Ha ocurrido un error inesperado. Por favor, intenta de nuevo.";
        public static string Unauthorized() => "No tienes permisos para realizar esta operación.";
        public static string EntityNotFound(string entityName, bool femine = false) => entityName + " no encontrad" + (femine ? "a" : "o") + ".";
        public static string EntitiesNotFound(string entitiesName, bool femine = false) => "Algun" + (femine ? "a" : "o") + " de l" + (femine ? "a" : "o") + "s " + entitiesName + " no pudo ser encontrad" + (femine ? "a" : "o") + ".";
        public static string FieldsRequired(string[] fields) => "Debes ingresar todos los campos obligatorios: " + string.Join(", ", fields);
        public static string FieldRequired(string fieldName) => "El campo " + fieldName + " es requerido.";
        public static string FieldGraterThanZero(string fieldName) => "El campo " + fieldName + " debe ser mayor a cero.";
        public static string FieldGraterOrEqualThanZero(string fieldName) => "El campo " + fieldName + " debe ser mayor o igual a cero.";
        public static string InvalidField(string fieldName) => "El campo " + fieldName + " no es válido.";
        public static string UniqueField(string entityName, string fieldName, bool femineField = false, bool femine = false) => "Ya existe un" + (femine ? "a" : "") + " " + entityName + " con es" + (femineField ? "a" : "e") + " " + fieldName + ".";
        public static string DuplicateEntity(string entity, bool femine = false) => "Ya existe un" + (femine ? "a" : "") + " " + entity + " con los datos ingresados.";
        public static string InvalidPassword() => "La contraseña debe contener al menos 8 caracteres, una letra mayúscula, una letra minúscula y un número.";
        public static string InvalidEmail() => "El email ingresado no es válido.";
        public static string InvalidLogin() => "Email y/o contraseña inválidos.";
        public static string ExpiredToken() => "El token ha expirado. Por favor, inicia sesión nuevamente.";
        public static string TokenCreation() => "Ha ocurrido un error al intentar crear el token. Por favor, intenta de nuevo.";
        public static string UserWithoutRole() => "El usuario con el que intentas acceder no posee un rol.";
        public static string NotEnoughStock(string productName) => "No hay suficiente stock de " + productName + ".";
    }

    public class CRUD
    {
        public static string EntityCreated(string entityName, bool femine = false) => entityName + " cread" + (femine ? "a" : "o") + " correctamente.";
        public static string EntityUpdated(string entityName, bool femine = false) => entityName + " editad" + (femine ? "a" : "o") + " correctamente.";
        public static string EntitiesUpdated(string entitiesName, bool femine = false) => entitiesName + " editad" + (femine ? "as" : "os") + " correctamente.";
        public static string EntityDeleted(string entityName, bool femine = false) => entityName + " eliminad" + (femine ? "a" : "o") + " correctamente.";
        public static string EntityDeactivated(string entityName, bool femine = false) => entityName + " desactivad" + (femine ? "a" : "o") + " correctamente.";
        public static string EntityActivated(string entityName, bool femine = false) => entityName + " activad" + (femine ? "a" : "o") + " correctamente.";
        public static string EntityAdded(string entityName, bool femine = false) => entityName + " añadid" + (femine ? "a" : "o") + " correctamente.";
    }
}
