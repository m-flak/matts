import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private authService: AuthService,
  ) {}

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
    // Token is in local storage but no login was performed thru interaction
    if (!this.authService.isLoggedIn() && this.authService.hasToken()) {
      try {
        const _ = await lastValueFrom(this.authService.loginUser());
        return true;
      } catch (e) {
        console.error(e);
        return this.router.createUrlTree(['/welcome/login']);
      }
    } else if (!this.authService.isLoggedIn()) {
      return this.router.createUrlTree(['/welcome/login']);
    }

    return true;
  }
}
