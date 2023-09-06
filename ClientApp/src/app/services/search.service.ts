import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment as env } from 'src/environments/environment';
import { Book } from 'src/models/Book';
import { PagedList } from 'src/types/pagedList';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  searchApiUrl = env.baseApiUrl + '/book/search';

  constructor(private http: HttpClient) {}

  public search(
    query: string,
    page: number,
    pageSize: number
  ): Observable<PagedList<Book>> {
    const params = new HttpParams()
      .set('query', query)
      .set('page', page)
      .set('pageSize', pageSize);

    console.log('Your search query has been submitted');

    return this.http.get<PagedList<Book>>(this.searchApiUrl, { params });
  }
}
