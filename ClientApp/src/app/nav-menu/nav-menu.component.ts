import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  homeTitle: string = '';
  isExpanded = false;

  ngOnInit(): void {
    this.homeTitle = sessionStorage.getItem('MenuBar_ActiveRole') ?? '';
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
