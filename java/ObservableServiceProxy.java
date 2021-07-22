/*-------------------------------------------------------------------------------------------------
| *** UNCLASSIFIED ***
|--------------------------------------------------------------------------------------------------
| U.S. Army Research, Development, and Engineering Command
| Aviation and Missile Research, Development, and Engineering Center
| Software Engineering Directorate, Redstone Arsenal, AL
|--------------------------------------------------------------------------------------------------
| Export-Control Act Warning: Warning - This Document contains technical data whose export is
| restricted by the Arms Export Control Act (Title 22, U.S.C., Sec 2751, et seq) or the Export
| Administration Act of 1979, as amended, Title 50, U.S.C., App. 2401 et seq. Violations of these
| export laws are subject to severe criminal penalties. Disseminate in accordance with provisions
| of DoD Directive 5230.25.
|--------------------------------------------------------------------------------------------------
| Distribution Statement C:  Distribution authorized to U.S. Government Agencies and their
| contractors, Export Controlled, Critical Technology, 13 Feb 2017. Other request for this document
| shall be referred to U.S. Army Aviation and Missile Research Development and Engineering Center
| Software Engineering Directorate, Attn: RDMR-BAW, Redstone Arsenal, AL 35898.
--------------------------------------------------------------------------------------------------*/

package mil.airforce.mc2.sdk.common.proxy;

import mil.airforce.mc2.sdk.common.ClientCredentials;

/**
 * Abstract base class for ServiceProxy subclasses that also subscribe to and observe messages.
 *
 * @param <I> the message type passed in to the subscribe method
 * @param <O> the message type being observed
 */
public abstract class ObservableServiceProxy<I, O> extends ServiceProxy implements IObservableServiceProxy<I, O>
{
  StreamObserverToRxObserver<O> responseObserver;

  /**
   * Constructor for our client proxy into a service.
   *
   * @param host        The IP address for where the service is running
   * @param port        The port on which the service is running
   * @param credentials Client information for all calls going to the server
   */
  ObservableServiceProxy(String host, int port, ClientCredentials credentials)
  {
    super(host, port, credentials);
  }

  /**
   * Close our channel to the service on disposal
   */
  @Override
  public void dispose()
  {
    if (!isDisposed())
    {
      completeObserver();
    }
    super.dispose();
  }

  /**
   * call onComplete on our observable stream
   */
  protected synchronized void completeObserver()
  {
    if (responseObserver != null)
    {
      responseObserver.onCompleted();
      responseObserver = null;
    }
  }
}