import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BookPageComponent } from './pages/book-page/book-page.component';
import { AuthorPageComponent } from './pages/author-page/author-page.component';

const routes: Routes = [
  {
    path: '',
    component: BookPageComponent,
  },
  {
    path: 'authors',
    component: AuthorPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
