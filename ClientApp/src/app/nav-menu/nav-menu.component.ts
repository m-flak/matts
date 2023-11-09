import { Component, OnInit, Input } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
})
export class NavMenuComponent implements OnInit {
  homeTitle: string = '';
  homeUser: string = '';
  isExpanded = false;

  @Input()
  hideNavItems = false;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.homeTitle = this.authService.currentUser?.role ?? '';
    this.homeUser = this.authService.currentUser?.name ?? '';
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  performLogout() {
    this.authService.logoutUser();
    this.router.navigate(['/welcome/login']);
  }
}
