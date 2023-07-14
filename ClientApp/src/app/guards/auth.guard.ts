import { Injectable } from "@angular/core";
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from "@angular/router";
import { environment } from './../../environments/environment';

@Injectable({
    providedIn: "root",
})
export class AuthGuard implements CanActivate {
    constructor(private router: Router) {}

    async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
        // TODO: Actually implement instead of using env json values
        const isAuthorized = environment.isAuthorized;
        const userAuthenticated = sessionStorage.getItem('authducktape') ?? 'false';
        if (!isAuthorized && userAuthenticated === 'false') {
            return this.router.createUrlTree(['/welcome/login']);
        }

        return true;
    }
}