
namespace WDT_assignment_1.Utilities
{
    public static class ExtensionUtilities
    {
        // inserts DBNull if there is no value present in obj
        public static object GetObjectOrDbNull(this object value) => value ?? DBNull.Value;
    }
}
