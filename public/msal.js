export { acquireTokenSilent, loginPopup, getAccount, logout }

const msalConfig = {
    auth: {
        clientId: "{you-client-id}",
        authority: "https://login.microsoftonline.com/{your-tenant-id}",
        validateAuthority: false,
        redirectUri: "http://localhost:3000/"
    },
    cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: false
    }
};

const msalInstance = new Msal.UserAgentApplication(msalConfig);

const loginRequest = {
    scopes: ["user.read"]
};

function acquireTokenSilent () {
    return msalInstance.acquireTokenSilent(loginRequest);
};

function loginPopup() {
    return msalInstance.loginPopup(loginRequest);
};

function getAccount() {
    var account = msalInstance.getAccount();
    return JSON.stringify(account);
};

function logout() {
    return msalInstance.logout();
};