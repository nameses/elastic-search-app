import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { catchError, of, throwError, timeout } from 'rxjs';
import { environment as env } from 'src/environments/environment';
import { LoadingState } from 'src/types/loadingState';
@Component({
  selector: 'app-generate',
  templateUrl: './generate.component.html',
  styleUrls: [],
})
export class GenerateComponent {
  generateApiUrl = env.baseApiUrl + '/book/generate/';
  dataLoadingState: LoadingState = { type: 'notStarted' };

  constructor(private http: HttpClient, private formBuilder: FormBuilder) {}

  generateForm: FormGroup = this.formBuilder.group({
    amount: 0,
  });

  onSubmit(): void {
    if (
      this.generateForm.get('amount')?.hasError('required') ||
      this.generateForm.get('amount')?.value == 0
    ) {
      return;
    }
    this.generate(this.generateForm.get('amount')?.value);
  }

  generate(amount: number): void {
    this.dataLoadingState = { type: 'loading' };

    this.http
      .post<any>(
        this.generateApiUrl + amount,
        {},
        { headers: {}, observe: 'response' }
      )
      .pipe(
        timeout(10000), // 10000 milliseconds = 10 seconds
        catchError((error) => {
          console.error('HTTP Error:', error);
          this.dataLoadingState = { type: 'loadedNotSuccessful' };
          return of(error);
        })
      )
      .subscribe((response) => {
        console.log(response);
        if (response && response.status >= 200 && response.status < 300)
          this.dataLoadingState = { type: 'loaded' };
        else this.dataLoadingState = { type: 'loadedNotSuccessful' };
      });
  }
}
