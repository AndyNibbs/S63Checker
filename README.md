# S63Checker
A tool to check that an S-63 exchange set is properly signed. 

Now includes checking for the proposed S63_SIGNATURES.XML file addition. 

The idea occurred to me during a CIRM ECDIS working group meeting where we were discussing possible extensions to the signing of S-63 chart exchange sets. 
- Could develop into ways for a mariner to check a dataset ahead of their ECDIS checking it.
- Could help in the creation of valid datasets.
- Can help highlight strengths and weaknesses of proposed change. 

## How to use it

Unzip the release. The exe is not signed so you may get warnings from Windows or antivirus when you try to run it. 

Run in cmd prompt with commands like this

```
s63checker e:\s63data\1.zip -verbose
s63checker e:\s63data\avcs_dvd_2.iso
s63checker e:\s63data\my_exchange_set
```

Andy Nibbs, Chersoft, December 2020. 
