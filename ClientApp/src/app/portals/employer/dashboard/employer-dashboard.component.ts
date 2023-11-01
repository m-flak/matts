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
import { of } from 'rxjs';
import { VlpProviderService } from 'src/app/components/view-list-panel';

@Component({
  selector: 'app-dashboard',
  templateUrl: './employer-dashboard.component.html',
  styleUrls: ['./employer-dashboard.component.scss'],
  providers: [
    VlpProviderService
  ]
})
export class EmployerDashboardComponent implements OnInit {
  displayedColumns: string[] = ['applicant', 'interviewer', 'position', 'actions'];

  constructor(
    private viewListPanelsData: VlpProviderService
  ) {}
  
  ngOnInit(): void {
    const tasks = [
      {
        name: "Task 1",
        description: "Task 1 requires your attention."
      },
      {
        name: "Task 2",
        description: "Task 2 requires your attention."
      }
    ];

    const messages = [
      {
        from: "Renee K.",
        body: "Fill out this checklist for me!"
      }
    ];

    this.viewListPanelsData.providers.set('tasks', {
      getItemsViewData: () => of(tasks)
    });
    this.viewListPanelsData.providers.set('messages', {
      getItemsViewData: () => of(messages)
    });
  }
}
