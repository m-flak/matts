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
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { ComponentsModule } from 'src/app/components/components.module';
import { PipesModule } from 'src/app/pipes/pipes.module';
import { EmployerDashboardRouteModule } from './employer-dashboard-route.module';
import { CommonModule } from '@angular/common';
import { EmployerDashboardComponent } from './employer-dashboard.component';

@NgModule({
  declarations: [EmployerDashboardComponent],
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    HttpClientModule,
    // Our modules start below
    EmployerDashboardRouteModule,
    ComponentsModule,
    PipesModule,
  ],
  exports: [EmployerDashboardComponent],
})
export class EmployerDashboardModule {}
