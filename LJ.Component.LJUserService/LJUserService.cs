using System.Net.Http;
using System.Security.Claims;

namespace LJ.Component.LJUserService
{
    public class LJUserService
    {
        //Public Properties
        public string? CloudId { get => _cloudId; }
        public bool IsAuthenticated { get => _isAuthenticated; }
        public int CountOfChanges { get => _countOfChanges; }


        //Private Fields
        private ClaimsPrincipal? _claimsPrincipal;
        private string? _cloudId;
        private bool _isAuthenticated;
        private HttpClient _clientLJUserService;
        private List<SimpleClaim>? _frontendClaims = new List<SimpleClaim>();
        private int _countOfChanges = 0;


        //Public Action
        public event Action? OnChange;

        //CONSTRUCTOR
        public LJUserService(IHttpClientFactory clientFactory)
        {
            _clientLJUserService = clientFactory.CreateClient("LJUserService");
        }


        //Public Methods
        public void UserJustLoggedIn(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;
            _countOfChanges++;

            //The CloudId is either the oid (AAD) or the sub (AADB2C) scope's value.
            _cloudId = getValueOfClaim("oid") ?? getValueOfClaim("sub");

            //If it has a CloudId, it is authenticated.
            _isAuthenticated = claimsPrincipal.Identity?.IsAuthenticated == true && CloudId != null;

            //populateClaimsFromBackend();

            populateClaimsFromFrontend();

            OnChange?.Invoke();
        }

        public void UserJustLoggedOut()
        {
            _cloudId = null;
            OnChange?.Invoke();
        }


        //Private Methods
        private string? getValueOfClaim(string type)
        {
            return _claimsPrincipal?.Claims?.Where(_ => _.Type == type)?.FirstOrDefault()?.Value;
        }

        private void populateClaimsFromFrontend()
        {
            _frontendClaims = new List<SimpleClaim>();

            foreach (var item in _claimsPrincipal!.Claims.OrderBy(_ => _.Type))
            {
                _frontendClaims.Add(new SimpleClaim() { type = item.Type, value = item.Value });
            }
        }
    }
}