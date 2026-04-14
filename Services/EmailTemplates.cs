

namespace PuppetFestAPP.Web.Services;
public static class EmailTemplates
{

    public static string ConfirmationEmailClean(string confirmationLink)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; padding:20px; background:#ffffff;'>

            <h2 style='color:#8B0000;'>PuppetFest</h2>

            <p>Please confirm your email to complete your account setup.</p>

            <p>
                <a href='{confirmationLink}' 
                   style='padding:10px 16px; background:#8B0000; color:white; text-decoration:none; border-radius:4px;'>
                   Confirm Account
                </a>
            </p>

            <p style='font-size:12px; color:#666;'>
                If you did not request this, please ignore this message.
            </p>

        </div>";
    }
}