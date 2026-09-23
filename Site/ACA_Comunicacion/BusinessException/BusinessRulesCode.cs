using Shared.ClassShared.BusinessExceptions;
using static Shared.ClassShared.Enum;

namespace Comunicaciones.BusinessExceptions
{
  public static class BusinessRulesCode
  {   
    private static BusinessRule personNotExists;
    private static BusinessRule personAlreadyExists;

    private static BusinessRule rolNotExists;
    private static BusinessRule rolAlreadyExists;

    private static BusinessRule zonaComercialNotExists;
    private static BusinessRule zonaComercialAlreadyExists;

    private static BusinessRule cuentaNotExists;
    private static BusinessRule cuentaAlreadyExists;

    private static BusinessRule centroNotExists;
    private static BusinessRule centroAlreadyExists;

    private static BusinessRule contactoNotExists;
    private static BusinessRule contactoAlreadyExists;

    private static BusinessRule listaContactosNotExists;
    private static BusinessRule listaContactosAlreadyExists;

    private static BusinessRule tipoContactoNotExists;
    private static BusinessRule tipoContactoAlreadyExists;

    private static BusinessRule cupoNotExists;
    private static BusinessRule cupoAlreadyExists;

    #region Persona
    public static BusinessRule PersonNotExists
    {
      get
      {
        return personNotExists
               ?? (personNotExists = new BusinessRule { Code = "PersonNotExists", Message = "La persona ingresada no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule PersonAlreadyExists
    {
      get
      {
        return personAlreadyExists
               ?? (personAlreadyExists = new BusinessRule { Code = "PersonAlreadyExists", Message = "La persona ingresada ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Rol
    public static BusinessRule RolNotExists
    {
      get
      {
        return rolNotExists
               ?? (rolNotExists = new BusinessRule { Code = "RolNotExists", Message = "El rol ingresado no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule RolAlreadyExists
    {
      get
      {
        return rolAlreadyExists
               ?? (rolAlreadyExists = new BusinessRule { Code = "RolAlreadyExists", Message = "El rol ingresado ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region ZonaComercial
    public static BusinessRule ZonaComercialNotExists
    {
      get
      {
        return zonaComercialNotExists
               ?? (zonaComercialNotExists = new BusinessRule { Code = "ZonaComercialNotExists", Message = "La zona comercial ingresada no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule ZonaComercialAlreadyExists
    {
      get
      {
        return zonaComercialAlreadyExists
               ?? (zonaComercialAlreadyExists = new BusinessRule { Code = "ZonaComercialAlreadyExists", Message = "La zona comercial ingresada ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Cuenta
    public static BusinessRule CuentaNotExists
    {
      get
      {
        return cuentaNotExists
               ?? (cuentaNotExists = new BusinessRule { Code = "CuentaNotExists", Message = "La cuenta ingresada no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule CuentaAlreadyExists
    {
      get
      {
        return centroAlreadyExists
               ?? (cuentaAlreadyExists= new BusinessRule { Code = "CuentaAlreadyExists", Message = "La cuenta ingresada ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Centro
    public static BusinessRule CentroNotExists
    {
      get
      {
        return centroNotExists
               ?? (centroNotExists = new BusinessRule { Code = "CentroNotExists", Message = "El centro ingresado no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule CentroAlreadyExists
    {
      get
      {
        return centroAlreadyExists
               ?? (centroAlreadyExists = new BusinessRule { Code = "CentroAlreadyExists", Message = "El centro ingresado ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Contacto
    public static BusinessRule ContactoNotExists
    {
      get
      {
        return contactoNotExists
               ?? (contactoNotExists = new BusinessRule { Code = "ContactoNotExists", Message = "El contacto ingresado no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule ContactoAlreadyExists
    {
      get
      {
        return contactoAlreadyExists
               ?? (contactoAlreadyExists= new BusinessRule { Code = "ContactoAlreadyExists", Message = "El contacto ingresado ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Lista Contacto
    public static BusinessRule ListaContactoNotExists
    {
      get
      {
        return listaContactosNotExists
               ?? (listaContactosNotExists = new BusinessRule { Code = "ListaContactoNotExists", Message = "La lista de contactos ingresada no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule ListaContactoAlreadyExists
    {
      get
      {
        return listaContactosAlreadyExists
               ?? (listaContactosAlreadyExists = new BusinessRule { Code = "ListaContactoAlreadyExists", Message = "La lista de contactos ingresada ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region TipoContacto
    public static BusinessRule TipoContactoNotExists
    {
      get
      {
        return tipoContactoNotExists
               ?? (tipoContactoNotExists = new BusinessRule { Code = "TipoContactoNotExists", Message = "El tipo de contacto ingresado no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule TipoContactoAlreadyExists
    {
      get
      {
        return tipoContactoAlreadyExists
               ?? (tipoContactoAlreadyExists = new BusinessRule { Code = "TipoContactoAlreadyExists", Message = "El tipo de contacto ingresado ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion

    #region Cupo
    public static BusinessRule CupoNotExists
    {
      get
      {
        return cupoNotExists
               ?? (cupoNotExists= new BusinessRule { Code = "CupoNotExists", Message = "El cupo ingresado no existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosNoExistente });
      }
    }
    public static BusinessRule CupoAlreadyExists
    {
      get
      {
        return cupoAlreadyExists
               ?? (cupoAlreadyExists = new BusinessRule { Code = "CupoAlreadyExists", Message = "El cupo ingresado ya existe.", TypeOfBusinessRule = TypeOfBusinessRule.DatosRepetidos });
      }
    }
    #endregion 

  }
}
