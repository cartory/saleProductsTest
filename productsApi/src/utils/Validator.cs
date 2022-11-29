using System.ComponentModel.DataAnnotations;

namespace src.utils
{
    public static class Validator
    {
        public static bool ValidateAtributes(object instance, out string errorMessage)
        {
            try
            {
                ValidationContext context = new ValidationContext(instance, null, null);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(instance, context, true);

                errorMessage = "";
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }
    }
}
