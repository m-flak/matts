/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 **/
import { Component, OnInit } from '@angular/core';
import { SIDE_MENU_CONFIG, SideMenuConfig } from 'src/app/components/side-menu/side-menu.component';

const menu: SideMenuConfig = {
  sections: [
    {
      items: [
        {
          text: 'Home',
          image: 'assets/sm-home.svg',
          imageHint: 'Home',
          route: '/employer',
        },
      ],
    },
    {
      items: [
        {
          text: 'Manage Jobs',
          image: 'assets/sm-manage-job.svg',
          imageHint: 'Manage Jobs',
          route: '/employer/jobs/list',
        },
        {
          text: 'Create Jobs',
          image: 'assets/sm-create-job.svg',
          imageHint: 'Create Jobs',
          route: '/employer/jobs/postNew',
        },
      ],
    },
    {
      items: [
        {
          text: 'Manage Applicants',
          image: 'assets/sm-manage-app.svg',
          imageHint: 'Manage Applicants',
          route: '/employer/applicants/list',
        },
        {
          text: 'Hiring / Onboarding',
          image: 'assets/sm-hiring.svg',
          imageHint: 'Hiring',
          route: '/employer/hiring',
        },
      ],
    },
    {
      items: [
        {
          text: 'Reports',
          image: 'assets/sm-reports.svg',
          imageHint: 'Reports',
          route: '/employer/reports',
        },
      ],
    },
    {
      items: [
        {
          text: 'Company',
          image: 'assets/sm-company.svg',
          imageHint: 'Company',
          route: '/employer/company',
        },
      ],
    },
    {
      items: [
        {
          text: 'Preferences',
          image: 'assets/sm-preferences.svg',
          imageHint: 'Preferences',
          route: '/employer/preferences',
        },
      ],
    },
  ],
};

@Component({
  selector: 'app-employer-root',
  templateUrl: './employer-root.component.html',
  styleUrls: ['./employer-root.component.scss'],
  providers: [{ provide: SIDE_MENU_CONFIG, useValue: menu }],
})
export class EmployerRootComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {}
}
