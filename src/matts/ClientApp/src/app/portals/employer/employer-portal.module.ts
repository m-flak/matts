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
import { ComponentsModule } from 'src/app/components/components.module';
import { PipesModule } from 'src/app/pipes/pipes.module';
import { EmployerPortalRouteModule } from './employer-portal-route.module';
import { CommonModule } from '@angular/common';
import { EmployerRootComponent } from './employer-root/employer-root.component';
import { UnderConstructionComponent } from './under-construction.component';

@NgModule({
  declarations: [EmployerRootComponent, UnderConstructionComponent],
  imports: [
    CommonModule,
    HttpClientModule,
    // Our modules start below
    EmployerPortalRouteModule,
    ComponentsModule,
    PipesModule,
  ],
  exports: [EmployerRootComponent, UnderConstructionComponent],
})
export class EmployerPortalModule {}
