import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-tests-errors',
  templateUrl: './tests-errors.component.html',
  styleUrls: ['./tests-errors.component.css'],
})
export class TestsErrorsComponent implements OnInit {
  baseUrl = 'https://localhost:5001/api/';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {}

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe(
      (resp) => {
        console.log(resp);
      },
      (err) => {
        console.log(err);
      }
    );
  }
}
