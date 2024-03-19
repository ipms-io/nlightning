# BOLT 8 Integration Tests

This repository contains integration tests for our project. These tests are designed to ensure that the various components of our system work together as expected.

The tests are based on the [Lightning Specification Bolt #8 Test Vectors](https://github.com/lightning/bolts/blob/08ce2f6f83619b777bebd86d6dff4a29096e35ae/08-transport.md#appendix-a-transport-test-vectors) from the Lightning Network's BOLTs repository.

Each of the following files represents tests from the Appendix A:

- [InitiatorIntegrationTests](InitiatorIntegrationTests.cs) - [Initiator Tests](https://github.com/lightning/bolts/blob/08ce2f6f83619b777bebd86d6dff4a29096e35ae/08-transport.md#initiator-tests)
- [ResponderIntegrationTests](InitiatorIntegrationTests.cs) - [Responder Tests](https://github.com/lightning/bolts/blob/08ce2f6f83619b777bebd86d6dff4a29096e35ae/08-transport.md#responder-tests)
- [MessageIntegrationTests](MessageIntegrationTests.cs) - [Message Encryption Tests](https://github.com/lightning/bolts/blob/08ce2f6f83619b777bebd86d6dff4a29096e35ae/08-transport.md#message-encryption-tests)

## Contributing

If you would like to contribute to these tests, please feel free to submit a pull request. We appreciate any and all help in ensuring that our system works as expected.