import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Author } from 'src/app/models/author';
import { environment } from 'src/environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class AuthorService {
  public BASE_URL = `${environment.api}/authors`;

  constructor(private _http: HttpClient) {}

  public getAuthors(): Observable<Array<Author>> {
    return this._http.get<Array<Author>>(this.BASE_URL);
  }

  public addAuthor(author: Author): Observable<Author> {
    return this._http.post<Author>(this.BASE_URL, author);
  }

  public updateAuthor(author: Author): Observable<Author> {
    return this._http.put<Author>(this.BASE_URL, author);
  }

  public removeAuthor(id: string): Observable<void> {
    return this._http.delete<void>(this.BASE_URL + '/' + id);
  }
}
