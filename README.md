# github-repository-analysis
Analysis of public github repositories

[![build and test](https://github.com/balanikas/github-repository-analysis/actions/workflows/deploy.yml/badge.svg)](https://github.com/balanikas/github-repository-analysis/actions/workflows/deploy.yml)
[![CodeQL](https://github.com/balanikas/github-repository-analysis/actions/workflows/codeql.yml/badge.svg)](https://github.com/balanikas/github-repository-analysis/actions/workflows/codeql.yml)

![Website](https://img.shields.io/website?down_message=offline&up_message=online&url=https%3A%2F%2Frd5iaiwd3y.us-west-2.awsapprunner.com%2F)



https://user-images.githubusercontent.com/2317329/205167273-49ee35ee-0695-4b4b-bf52-606bc7e91421.mov



This service is primarily targeted to small or medium sized repos,
that are owned by a single person or a small group of contributors.
It can be very useful to those getting started with open source projects, but also
for more experienced developers who want to do a quality check of their own repos.

The analysis is presented as a set of detections, where each detection is either
- Okay - looks good and no action needed 
- Can be improved - an action can be taken to improve the repo 
- Warning - strongly recommended to address this

This service currently has extra checks for csharp repositories, but any repository should work.
For feedback, please create a new issue at https://github.com/balanikas/github-repository-analysis/issues

site: https://githubrepositoryanalysis.com/
